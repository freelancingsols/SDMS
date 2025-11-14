import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizeService } from '../../auth/authorize.service';

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
