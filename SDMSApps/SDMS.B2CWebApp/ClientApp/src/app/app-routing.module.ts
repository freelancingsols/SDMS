import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TestComponent } from './Components/test/test.component';

const routes: Routes = [
  { path: 'test', component: TestComponent },
  { path: '', redirectTo: '/test', pathMatch: 'full' },
  { path: '**', redirectTo: '/test' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
