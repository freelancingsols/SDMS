import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-center-canvas',
  templateUrl: './center-canvas.component.html',
  styleUrls: ['./center-canvas.component.css']
})
export class CenterCanvasComponent implements OnInit, OnChanges {
  @Input() componentToLoad: string = 'test';
  currentComponent: string = 'test';

  ngOnInit() {
    // Load test component by default
    this.currentComponent = this.componentToLoad || 'test';
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['componentToLoad'] && changes['componentToLoad'].currentValue) {
      this.currentComponent = changes['componentToLoad'].currentValue;
    }
  }

  loadComponent(componentName: string) {
    this.currentComponent = componentName;
  }
}