import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  @Output() toggleLeftSidebar = new EventEmitter<void>();
  @Output() toggleRightSidebar = new EventEmitter<void>();
  @Output() loadComponent = new EventEmitter<string>();
  
  isCollapsed = false;

  onToggleLeftSidebar() {
    this.toggleLeftSidebar.emit();
  }

  onToggleRightSidebar() {
    this.toggleRightSidebar.emit();
  }

  onLoadComponent(componentName: string) {
    this.loadComponent.emit(componentName);
  }
}
