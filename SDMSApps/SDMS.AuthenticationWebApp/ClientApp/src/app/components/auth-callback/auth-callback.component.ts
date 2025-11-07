import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="callback-container">
      <div class="callback-card">
        <div *ngIf="isLoading" class="loading">
          <p>Processing authentication...</p>
        </div>
        <div *ngIf="error" class="error">
          <p>Authentication failed: {{ error }}</p>
          <button (click)="goToLogin()">Go to Login</button>
        </div>
        <div *ngIf="success" class="success">
          <p>Authentication successful!</p>
          <p>Redirecting to profile...</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .callback-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: calc(100vh - 200px);
      padding: 2rem;
    }
    .callback-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
      padding: 2rem;
      text-align: center;
    }
    .loading, .error, .success {
      padding: 1rem;
    }
    .error {
      color: #d32f2f;
    }
    .success {
      color: #2e7d32;
    }
    button {
      margin-top: 1rem;
      padding: 0.75rem 1.5rem;
      background: #1976d2;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }
  `]
})
export class AuthCallbackComponent implements OnInit {
  isLoading = true;
  error: string | null = null;
  success = false;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private oauthService: OAuthService
  ) {}

  async ngOnInit() {
    try {
      // Check for authorization code in query params
      this.route.queryParams.subscribe(async params => {
        const code = params['code'];
        const state = params['state'];
        const error = params['error'];

        if (error) {
          this.error = error;
          this.isLoading = false;
          return;
        }

        if (code) {
          try {
            // Complete the OAuth flow
            await this.oauthService.loadDiscoveryDocument();
            const success = await this.oauthService.tryLoginCodeFlow();
            
            if (success && this.oauthService.hasValidAccessToken()) {
              await this.authService.loadUserProfile();
              this.success = true;
              setTimeout(() => {
                this.router.navigate(['/profile']);
              }, 1000);
            } else {
              this.error = 'Failed to complete authentication';
            }
          } catch (err: any) {
            this.error = err.message || 'Authentication error';
          } finally {
            this.isLoading = false;
          }
        } else {
          // No code parameter, might be a direct callback
          if (this.oauthService.hasValidAccessToken()) {
            await this.authService.loadUserProfile();
            this.router.navigate(['/profile']);
          } else {
            this.error = 'No authorization code received';
            this.isLoading = false;
          }
        }
      });
    } catch (err: any) {
      this.error = err.message || 'Unexpected error';
      this.isLoading = false;
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}

