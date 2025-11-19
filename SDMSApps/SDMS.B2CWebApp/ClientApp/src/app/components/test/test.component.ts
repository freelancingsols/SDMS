import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizeService } from '../../auth/authorize.service';
import { AuthService, UserInfo } from '../../services/auth.service';
import { OAuthService } from 'angular-oauth2-oidc';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.css']
})
export class TestComponent implements OnInit, OnDestroy {
  public username: string = '';
  public userInfo: UserInfo | null = null;
  public isLoading = false;
  public tokenInfo: any = null;
  private userSubscription?: Subscription;
  private userInfoSubscription?: Subscription;

  constructor(
    private router: Router,
    private authorizeService: AuthorizeService,
    private authService: AuthService,
    private oauthService: OAuthService
  ) { }

  async ngOnInit() {
    this.isLoading = true;
    
    // Load user from AuthorizeService (legacy)
    this.userSubscription = this.authorizeService.getUser().subscribe(user => {
      if (user && user.name) {
        this.username = user.name;
      } else {
        this.username = '';
      }
    });

    // Load detailed user info from AuthService
    this.userInfoSubscription = this.authService.userInfo$.subscribe(userInfo => {
      this.userInfo = userInfo;
      if (userInfo?.displayName) {
        this.username = userInfo.displayName;
      } else if (userInfo?.email) {
        this.username = userInfo.email;
      }
      this.isLoading = false;
    });

    // Load token information
    this.loadTokenInfo();
    
    // Ensure user profile is loaded
    await this.authService.loadUserProfile();
    this.isLoading = false;
  }

  ngOnDestroy() {
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
    if (this.userInfoSubscription) {
      this.userInfoSubscription.unsubscribe();
    }
  }

  private loadTokenInfo() {
    try {
      const accessToken = this.oauthService.getAccessToken();
      const idToken = this.oauthService.getIdToken();
      const refreshToken = this.oauthService.getRefreshToken();
      const hasValidToken = this.oauthService.hasValidAccessToken();
      const claims = this.oauthService.getIdentityClaims();

      if (accessToken || idToken) {
        this.tokenInfo = {
          hasAccessToken: !!accessToken,
          hasIdToken: !!idToken,
          hasRefreshToken: !!refreshToken,
          isValid: hasValidToken,
          accessTokenLength: accessToken?.length || 0,
          idTokenLength: idToken?.length || 0,
          refreshTokenLength: refreshToken?.length || 0,
          claims: claims ? Object.keys(claims).length : 0,
          expiresAt: this.getTokenExpiration()
        };
      }
    } catch (error) {
      console.error('Error loading token info:', error);
    }
  }

  private getTokenExpiration(): string | null {
    try {
      const expiresAt = sessionStorage.getItem('access_token_expires_at');
      if (expiresAt) {
        const expirationDate = new Date(parseInt(expiresAt) * 1000);
        const now = new Date();
        const diff = expirationDate.getTime() - now.getTime();
        const minutes = Math.floor(diff / 60000);
        return minutes > 0 ? `${minutes} minutes` : 'Expired';
      }
      return null;
    } catch {
      return null;
    }
  }

  public loadTest() {
    this.router.navigateByUrl('/login', {
      replaceUrl: true
    });
  }

  public loadTestComponent() {
    this.isLoading = true;
    this.router.navigateByUrl('/test', {
      replaceUrl: false
    }).then(() => {
      this.isLoading = false;
    });
  }

  public async refreshUserInfo() {
    this.isLoading = true;
    try {
      await this.authService.loadUserProfile();
      this.loadTokenInfo();
    } catch (error) {
      console.error('Error refreshing user info:', error);
    } finally {
      this.isLoading = false;
    }
  }

  public async refreshToken() {
    this.isLoading = true;
    try {
      const success = await this.authService.refreshToken();
      if (success) {
        this.loadTokenInfo();
        alert('Token refreshed successfully!');
      } else {
        alert('Failed to refresh token. Please login again.');
      }
    } catch (error) {
      console.error('Error refreshing token:', error);
      alert('Error refreshing token');
    } finally {
      this.isLoading = false;
    }
  }

  public async logout() {
    this.isLoading = true;
    try {
      this.username = '';
      this.userInfo = null;
      this.tokenInfo = null;
      
      await this.authorizeService.signOut({ returnUrl: '/' });
      await new Promise(resolve => setTimeout(resolve, 300));
      
      this.router.navigate(['/'], { replaceUrl: true });
    } catch (error) {
      console.error('Error during logout:', error);
      this.router.navigate(['/'], { replaceUrl: true });
    } finally {
      this.isLoading = false;
    }
  }
}
