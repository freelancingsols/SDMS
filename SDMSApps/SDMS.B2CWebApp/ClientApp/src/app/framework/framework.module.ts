import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

// Framework components
import { FrameworkBodyComponent } from './framework-body/framework-body.component';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';
import { ContentComponent } from './content/content.component';

// Material modules
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';

// Import sidebar components
import { LeftSidebarComponent } from './left-sidebar/left-sidebar.component';
import { RightSidebarComponent } from './right-sidebar/right-sidebar.component';
import { CenterCanvasComponent } from './center-canvas/center-canvas.component';

// Import test component (used by center-canvas)
import { TestComponent } from '../components/test/test.component';

@NgModule({
  declarations: [
    FrameworkBodyComponent,
    HeaderComponent,
    FooterComponent,
    ContentComponent,
    LeftSidebarComponent,
    RightSidebarComponent,
    CenterCanvasComponent,
    TestComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatDividerModule,
    MatListModule,
    MatSidenavModule,
    MatTooltipModule,
    MatCheckboxModule
  ],
  exports: [
    FrameworkBodyComponent,
    HeaderComponent,
    FooterComponent,
    TestComponent  // Export TestComponent so it can be used in routing
  ]
})
export class FrameworkModule { }
