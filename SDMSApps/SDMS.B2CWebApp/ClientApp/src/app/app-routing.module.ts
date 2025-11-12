import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TestComponent } from './Components/test/test.component';

const routes: Routes = [
  { path: 'test', component: TestComponent },
  { path: 'auth-callback', component: TestComponent }, // OAuth callback - OAuthService handles it automatically
  { path: '', redirectTo: '/test', pathMatch: 'full' },
  { path: '**', redirectTo: '/test' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
