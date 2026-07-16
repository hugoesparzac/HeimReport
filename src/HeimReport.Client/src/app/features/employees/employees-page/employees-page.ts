import { Component , inject } from '@angular/core';
import { Router } from '@angular/router';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TopbarComponent } from '../../../shared/components/topbar/topbar';
import { PageBackgroundComponent } from '../../../shared/components/page-background/page-background';
import { ButtonComponent } from '../../../shared/components/button/button';
import { WorkforceOverviewBannerComponent } from './components/workforce-overview-banner/workforce-overview-banner';
import { EmployeeDirectoryTableComponent } from './components/employee-directory-table/employee-directory-table';
import { TopSeniorityPanelComponent } from './components/top-seniority-panel/top-seniority-panel';

@Component({
  selector: 'app-employees-page',
  standalone: true,
  imports: [
    SidebarComponent,
    TopbarComponent,
    PageBackgroundComponent,
    ButtonComponent,
    WorkforceOverviewBannerComponent,
    EmployeeDirectoryTableComponent,
    TopSeniorityPanelComponent,
  ],
  templateUrl: './employees-page.html',
})
export class EmployeesPageComponent {

    private router = inject(Router);

  goToRegister(): void {
    this.router.navigate(['/employees/register']);
  }
}