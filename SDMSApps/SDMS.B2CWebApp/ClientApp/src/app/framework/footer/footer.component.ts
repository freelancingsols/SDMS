import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent {
  @Input() isCollapsed = false;
  @Output() loadComponent = new EventEmitter<string>();
  currentYear = new Date().getFullYear();

  onLoadComponent(componentName: string) {
    this.loadComponent.emit(componentName);
  }
}
