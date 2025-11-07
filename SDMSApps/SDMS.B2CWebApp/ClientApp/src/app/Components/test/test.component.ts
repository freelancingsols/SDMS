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
    { id: 1, name: 'Item 1', description: 'Description for item 1', price: 29.99 },
    { id: 2, name: 'Item 2', description: 'Description for item 2', price: 39.99 },
    { id: 3, name: 'Item 3', description: 'Description for item 3', price: 49.99 }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit() {
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
