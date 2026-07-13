import { Component } from '@angular/core';
import { ButtonComponent } from '../../../../../shared/components/button/button';

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [ButtonComponent],
  templateUrl: './login-form.html',
  styleUrls: ['./login-form.css']
})
export class LoginFormComponent {}