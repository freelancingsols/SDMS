import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterLink],
  template: `
    <div class="app-container">
      <header class="app-header">
        <h1>SDMS Authentication</h1>
        <nav>
          <a routerLink="/login" *ngIf="!isAuthenticated">Login</a>
          <a routerLink="/register" *ngIf="!isAuthenticated">Register</a>
          <a routerLink="/profile" *ngIf="isAuthenticated">Profile</a>
          <button (click)="logout()" *ngIf="isAuthenticated">Logout</button>
        </nav>
      </header>
      <main class="app-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }
    .app-header {
      background: #1976d2;
      color: white;
      padding: 1rem 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .app-header h1 {
      margin: 0;
      font-size: 1.5rem;
    }
    .app-header nav {
      display: flex;
      gap: 1rem;
      align-items: center;
    }
    .app-header a {
      color: white;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      transition: background 0.3s;
    }
    .app-header a:hover {
      background: rgba(255, 255, 255, 0.1);
    }
    .app-header button {
      background: rgba(255, 255, 255, 0.2);
      color: white;
      border: 1px solid white;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
    }
    .app-header button:hover {
      background: rgba(255, 255, 255, 0.3);
    }
    .app-content {
      flex: 1;
    }
  `]
})
export class AppComponent implements OnInit {
  isAuthenticated = false;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.authService.userInfo$.subscribe(userInfo => {
      this.isAuthenticated = userInfo != null || this.authService.isAuthenticated();
    });
  }

  async logout() {
    await this.authService.logout();
  }
}
