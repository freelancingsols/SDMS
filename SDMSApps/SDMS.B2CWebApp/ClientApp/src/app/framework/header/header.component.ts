import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  @Input() isCollapsed = false;
  @Output() toggleLeftSidebar = new EventEmitter<void>();
  @Output() toggleRightSidebar = new EventEmitter<void>();
  @Output() loadComponent = new EventEmitter<string>();

  searchQuery: string = '';

  onToggleLeftSidebar() {
    this.toggleLeftSidebar.emit();
  }

  onToggleRightSidebar() {
    this.toggleRightSidebar.emit();
  }

  onLoadComponent(componentName: string) {
    this.loadComponent.emit(componentName);
  }

  onSearch() {
    if (this.searchQuery.trim()) {
      console.log('Searching for:', this.searchQuery);
      // Implement search functionality here
    }
  }

  clearSearch() {
    this.searchQuery = '';
  }
}
