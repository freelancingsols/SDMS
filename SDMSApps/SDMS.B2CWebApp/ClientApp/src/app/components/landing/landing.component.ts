import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class LandingComponent {
  constructor(private router: Router) { }

  onGetStarted() {
    // Navigate to /test - AuthorizeGuard will redirect to login if not authenticated
    // After successful login, user will be redirected back to /test
    this.router.navigate(['/test']);
  }
}

