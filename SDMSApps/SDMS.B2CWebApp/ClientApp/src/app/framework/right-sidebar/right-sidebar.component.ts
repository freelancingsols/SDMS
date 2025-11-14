import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-right-sidebar',
  templateUrl: './right-sidebar.component.html',
  styleUrls: ['./right-sidebar.component.css']
})
export class RightSidebarComponent implements OnInit, OnChanges {
  @Input() isCollapsed = false;
  @Output() loadComponent = new EventEmitter<string>();

  ngOnInit() {
    console.log('RightSidebar ngOnInit - isCollapsed:', this.isCollapsed);
  }

  ngOnChanges(changes: SimpleChanges) {
    console.log('RightSidebar ngOnChanges - changes:', changes);
    if (changes['isCollapsed']) {
      console.log('RightSidebar isCollapsed changed from', changes['isCollapsed'].previousValue, 'to', changes['isCollapsed'].currentValue);
    }
  }

  onLoadComponent(componentName: string) {
    this.loadComponent.emit(componentName);
  }
}