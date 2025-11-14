import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-left-sidebar',
  templateUrl: './left-sidebar.component.html',
  styleUrls: ['./left-sidebar.component.css']
})
export class LeftSidebarComponent implements OnInit, OnChanges {
  @Input() isCollapsed = false;
  @Output() loadComponent = new EventEmitter<string>();
  activeMenu: string = 'dashboard';

  ngOnInit() {
    console.log('LeftSidebar ngOnInit - isCollapsed:', this.isCollapsed);
  }

  ngOnChanges(changes: SimpleChanges) {
    console.log('LeftSidebar ngOnChanges - changes:', changes);
    if (changes['isCollapsed']) {
      console.log('LeftSidebar isCollapsed changed from', changes['isCollapsed'].previousValue, 'to', changes['isCollapsed'].currentValue);
    }
  }

  onLoadComponent(componentName: string) {
    this.activeMenu = componentName;
    this.loadComponent.emit(componentName);
  }
}