import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-content',
  templateUrl: './content.component.html',
  styleUrls: ['./content.component.css']
})
export class ContentComponent implements OnInit {
  // Dummy data for UI display
  featuredItems = [
    { id: 1, title: 'Featured Product 1', description: 'Description for featured product 1', image: 'assets/11.jpg' },
    { id: 2, title: 'Featured Product 2', description: 'Description for featured product 2', image: 'assets/12.jpg' },
    { id: 3, title: 'Featured Product 3', description: 'Description for featured product 3', image: 'assets/13.jpg' }
  ];

  constructor() { }

  ngOnInit() {
    // Using dummy data instead of API calls
    console.log('Content component loaded with dummy data');
  }
}
