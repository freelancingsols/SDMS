import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizeService, AuthenticationResultStatus } from '../../auth/authorize.service';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.css']
})
export class TestComponent implements OnInit {
  public username: string = '';

  constructor(
    private router: Router,
    private authorizeService: AuthorizeService
  ) { }

  async ngOnInit() {
    // Check if this is the OAuth callback route
    const currentUrl = this.router.url;
    if (currentUrl.includes('/auth-callback')) {
      // Let OAuthService handle the callback
      try {
        const result = await this.authorizeService.completeSignIn(window.location.href, 'redirect');
        // After callback, check if authenticated and redirect
        if (result.status === AuthenticationResultStatus.Success) {
          // Wait a bit for token to be processed
          await new Promise(resolve => setTimeout(resolve, 100));
          const isAuthenticated = await this.authorizeService.isAuthenticated().pipe(take(1)).toPromise();
          if (isAuthenticated) {
            this.router.navigate(['/test'], { replaceUrl: true });
          } else {
            this.router.navigate(['/login'], { replaceUrl: true });
          }
        } else {
          const errorMessage = 'message' in result ? result.message : 'Authentication failed';
          console.error('Authentication failed:', errorMessage);
          this.router.navigate(['/login'], { replaceUrl: true });
        }
      } catch (error) {
        console.error('Error handling OAuth callback:', error);
        this.router.navigate(['/login'], { replaceUrl: true });
      }
      return;
    }

    // For regular routes, check authentication (optional - can be removed if not needed)
    // const isAuthenticated = await this.authorizeService.isAuthenticated().pipe(take(1)).toPromise();
    // if (!isAuthenticated) {
    //   this.router.navigate(['/login'], {
    //     queryParams: { returnUrl: this.router.url }
    //   });
    //   return;
    // }

    // Load user if authenticated
    this.authorizeService.getUser().subscribe(user => {
      if (user && user.name) {
        this.username = user.name;
      }
    });
  }

  public loadTest() {
    this.router.navigateByUrl('/login', {
      replaceUrl: true
    });
  }

  public loadTestComponent() {
    // Reload the test component by navigating to the test route
    this.router.navigateByUrl('/test', {
      replaceUrl: false
    });
  }

  public async logout() {
    try {
      // Clear username immediately
      this.username = '';
      
      // Call signOut - it will clear OAuth tokens and user state
      await this.authorizeService.signOut({ returnUrl: '/' });
      
      // Wait a bit for OAuth service to process logout
      await new Promise(resolve => setTimeout(resolve, 100));
      
      // Navigate to login page
      this.router.navigate(['/login'], { replaceUrl: true });
    } catch (error) {
      console.error('Error during logout:', error);
      // Still navigate to login even if logout fails
      this.username = '';
      this.router.navigate(['/login'], { replaceUrl: true });
    }
  }
}
