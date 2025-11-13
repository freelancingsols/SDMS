import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-header-new',
  templateUrl: './header-new.component.html',
  styleUrls: ['./header-new.component.css']
})
export class HeaderNewComponent {
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
