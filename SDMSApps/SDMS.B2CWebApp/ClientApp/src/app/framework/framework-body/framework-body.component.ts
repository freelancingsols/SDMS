import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-framework-body',
  templateUrl: './framework-body.component.html',
  styleUrls: ['./framework-body.component.css']
})
export class FrameworkBodyComponent implements OnInit {
  leftSidebarCollapsed = false;
  rightSidebarCollapsed = false;
  headerCollapsed = false;
  footerCollapsed = false;
  currentComponent: string = 'test';

  constructor() { }

  ngOnInit() {
    console.log('FrameworkBody initialized');
    console.log('leftSidebarCollapsed:', this.leftSidebarCollapsed);
    console.log('rightSidebarCollapsed:', this.rightSidebarCollapsed);
  }

  onToggleLeftSidebar() {
    this.leftSidebarCollapsed = !this.leftSidebarCollapsed;
  }

  onToggleRightSidebar() {
    this.rightSidebarCollapsed = !this.rightSidebarCollapsed;
  }

  onToggleHeader() {
    this.headerCollapsed = !this.headerCollapsed;
  }

  onToggleFooter() {
    this.footerCollapsed = !this.footerCollapsed;
  }

  onLoadComponent(componentName: string) {
    this.currentComponent = componentName;
  }
}
