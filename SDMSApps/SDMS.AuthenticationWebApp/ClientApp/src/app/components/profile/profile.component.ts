import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService, UserInfo } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  userInfo: UserInfo | null = null;
  isLoading = true;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async ngOnInit() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    try {
      await this.authService.loadUserProfile();
      this.authService.userInfo$.subscribe(userInfo => {
        this.userInfo = userInfo;
        this.isLoading = false;
      });
    } catch (error) {
      console.error('Error loading profile:', error);
      this.isLoading = false;
    }
  }

  async logout() {
    await this.authService.logout();
  }
}

