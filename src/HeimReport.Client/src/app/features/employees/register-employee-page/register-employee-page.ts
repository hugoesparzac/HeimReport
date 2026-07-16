import { Component } from '@angular/core';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TopbarComponent } from '../../../shared/components/topbar/topbar';
import { PageBackgroundComponent } from '../../../shared/components/page-background/page-background';
import { ButtonComponent } from '../../../shared/components/button/button';
import { FormSectionComponent } from '../../../shared/components/form-section/form-section';
import { ProfilePhotoUploadComponent } from './components/profile-photo-upload/profile-photo-upload';

@Component({
  selector: 'app-register-employee-page',
  standalone: true,
  imports: [
    SidebarComponent,
    TopbarComponent,
    PageBackgroundComponent,
    ButtonComponent,
    FormSectionComponent,
    ProfilePhotoUploadComponent,
  ],
  templateUrl: './register-employee-page.html',
})
export class RegisterEmployeePageComponent {}