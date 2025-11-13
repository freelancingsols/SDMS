import { Component, OnInit, ViewChild } from '@angular/core';
import { CenterCanvasComponent } from '../../Components/center-canvas/center-canvas.component';

@Component({
  selector: 'app-framework-body',
  templateUrl: './framework-body.component.html',
  styleUrls: ['./framework-body.component.css']
})
export class FrameworkBodyComponent implements OnInit {
  @ViewChild('centerCanvas') centerCanvas!: CenterCanvasComponent;
  
  leftSidebarCollapsed = false;
  rightSidebarCollapsed = false;

  constructor() { }

  ngOnInit() {
  }

  onToggleLeftSidebar() {
    this.leftSidebarCollapsed = !this.leftSidebarCollapsed;
  }

  onToggleRightSidebar() {
    this.rightSidebarCollapsed = !this.rightSidebarCollapsed;
  }

  onLoadComponent(componentName: string) {
    if (this.centerCanvas) {
      this.centerCanvas.loadComponent(componentName);
    }
  }
}
