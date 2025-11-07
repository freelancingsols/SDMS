import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FrameworkBodyComponent } from './framework-body/framework-body.component';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { FooterComponent } from './footer/footer.component';
import { ContentComponent } from './content/content.component';



@NgModule({
  declarations: [FrameworkBodyComponent, HeaderComponent, FooterComponent, ContentComponent],
  imports: [
    CommonModule,
    RouterModule
  ],
  exports: [
    FrameworkBodyComponent
  ]
})
export class FrameworkModule { }
