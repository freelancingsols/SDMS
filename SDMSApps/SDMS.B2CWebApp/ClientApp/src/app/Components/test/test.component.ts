import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.css']
})
export class TestComponent implements OnInit {
  // Dummy data instead of API calls
  username: string = 'John Doe';
  userInfo: any = {
    userId: '123',
    email: 'john.doe@example.com',
    displayName: 'John Doe',
    roles: ['User']
  };
  
  // Dummy data for display
  items = [
    { id: 1, name: 'Premium Headphones', description: 'High-quality wireless headphones with noise cancellation', price: 199.99 },
    { id: 2, name: 'Smart Watch', description: 'Feature-rich smartwatch with health tracking', price: 299.99 },
    { id: 3, name: 'Laptop Stand', description: 'Ergonomic aluminum laptop stand for better posture', price: 49.99 },
    { id: 4, name: 'Wireless Mouse', description: 'Ergonomic wireless mouse with long battery life', price: 39.99 },
    { id: 5, name: 'Mechanical Keyboard', description: 'RGB backlit mechanical keyboard with blue switches', price: 89.99 },
    { id: 6, name: 'USB-C Hub', description: 'Multi-port USB-C hub with HDMI and SD card reader', price: 59.99 }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit() {
    // Check if this is an OAuth callback
    const currentUrl = this.router.url;
    if (currentUrl.includes('/auth-callback')) {
      // OAuth callback - let the OAuthService handle it
      // The OAuthService should process the callback automatically
      // After processing, redirect to home
      setTimeout(() => {
        if (this.authService.isAuthenticated()) {
          this.router.navigate(['/test']);
        } else {
          this.router.navigate(['/test']);
        }
      }, 1000);
      return;
    }
    
    // Use dummy data instead of API call
    // If authenticated, get user info from auth service
    if (this.authService.isAuthenticated()) {
      const userInfo = this.authService.getUserInfo();
      if (userInfo) {
        this.userInfo = userInfo;
        this.username = userInfo.displayName || userInfo.email || 'User';
      }
    }
  }

  public loadTest() {
    this.router.navigateByUrl('/login', {
      replaceUrl: true
    });
  }
}
