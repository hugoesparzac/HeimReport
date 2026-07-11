import { Component } from '@angular/core';
import { LoginFormComponent } from '../../ui/login-form/login-form.component';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [LoginFormComponent], // <--- Añádelo aquí
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css']
})
export class LoginPageComponent {}