import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit {
  leftSidebarCollapsed = false;
  rightSidebarCollapsed = false;
  currentComponent: string = '';

  constructor() { }

  ngOnInit(): void {
  }

  toggleLeftSidebar(): void {
    this.leftSidebarCollapsed = !this.leftSidebarCollapsed;
  }

  toggleRightSidebar(): void {
    this.rightSidebarCollapsed = !this.rightSidebarCollapsed;
  }

  onLoadComponent(componentName: string): void {
    this.currentComponent = componentName;
  }
}
