import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizeService } from '../../auth/authorize.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.css']
})
export class TestComponent implements OnInit, OnDestroy {
  public username: string = '';
  private userSubscription?: Subscription;

  constructor(
    private router: Router,
    private authorizeService: AuthorizeService
  ) { }

  async ngOnInit() {
    // Load user if authenticated
    this.userSubscription = this.authorizeService.getUser().subscribe(user => {
      if (user && user.name) {
        this.username = user.name;
      } else {
        // Clear username if user is null
        this.username = '';
      }
    });
  }

  ngOnDestroy() {
    // Unsubscribe to prevent memory leaks
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
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
      
      // Wait a bit for OAuth service to process logout and clear storage
      await new Promise(resolve => setTimeout(resolve, 300));
      
      // Ensure username is cleared (in case subscription updates it)
      this.username = '';
      
      // Navigate to login page
      this.router.navigate(['/login'], { replaceUrl: true });
    } catch (error) {
      console.error('Error during logout:', error);
      // Still clear username and navigate to login even if logout fails
      this.username = '';
      this.router.navigate(['/login'], { replaceUrl: true });
    }
  }
}
