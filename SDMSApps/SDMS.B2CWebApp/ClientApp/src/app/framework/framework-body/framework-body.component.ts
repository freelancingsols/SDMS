import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

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
  currentComponent: string | null = null; // Clear by default - router-outlet will show routed components

  constructor(private router: Router) { }

  ngOnInit() {
    console.log('FrameworkBody initialized');
    console.log('leftSidebarCollapsed:', this.leftSidebarCollapsed);
    console.log('rightSidebarCollapsed:', this.rightSidebarCollapsed);
    
    // Clear currentComponent when route changes (so router-outlet takes precedence)
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.currentComponent = null;
      });
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
    // When loading a component dynamically, set currentComponent
    // This will show app-center-canvas instead of router-outlet
    this.currentComponent = componentName;
  }
}
