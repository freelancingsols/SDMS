import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  async onEmailLogin() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter email and password';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    try {
      const success = await this.authService.loginWithEmail(this.email, this.password);
      if (success) {
        // Get the ReturnUrl from query parameters (used in OAuth flow)
        const returnUrl = this.route.snapshot.queryParams['ReturnUrl'] || '/profile';
        // Use window.location.href for full page redirect to preserve OAuth flow state
        // This will typically be /connect/authorize?... which will complete the OAuth flow
        // Full page redirect is necessary to ensure OpenIddict can properly handle the authorization request
        window.location.href = decodeURIComponent(returnUrl);
      } else {
        this.errorMessage = 'Invalid email or password';
      }
    } catch (error) {
      this.errorMessage = 'Login failed. Please try again.';
    } finally {
      this.isLoading = false;
    }
  }

  async onExternalLogin(provider: 'auth0' | 'google') {
    this.isLoading = true;
    try {
      await this.authService.loginWithExternalProvider(provider);
    } catch (error) {
      this.errorMessage = `Failed to initiate ${provider} login`;
      this.isLoading = false;
    }
  }
}

