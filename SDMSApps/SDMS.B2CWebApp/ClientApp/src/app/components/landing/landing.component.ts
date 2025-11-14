import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class LandingComponent implements OnInit {
  constructor(private router: Router) { }

  ngOnInit() {
    // Clear any OAuth callback parameters from URL if present
    // This prevents logout callbacks from being processed as login callbacks
    const urlParams = new URLSearchParams(window.location.search);
    const hasIss = urlParams.has('iss');
    const hasCode = urlParams.has('code');
    const hasState = urlParams.has('state');
    
    // If we have OAuth parameters but no code/state (logout callback), clear them
    if (hasIss && !hasCode && !hasState) {
      window.history.replaceState({}, document.title, '/');
    }
  }

  onGetStarted() {
    // Navigate to /test - AuthorizeGuard will redirect to login if not authenticated
    // After successful login, user will be redirected back to /test
    this.router.navigate(['/test']);
  }
}

