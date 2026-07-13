import { Component } from '@angular/core';
import { ButtonComponent } from '../../../../../shared/components/button/button';
import { StatusBadgeComponent } from '../../../../../shared/components/status-badge/status-badge';

@Component({
  selector: 'app-employees-at-risk-table',
  standalone: true,
  imports: [ButtonComponent, StatusBadgeComponent],
  templateUrl: './employees-at-risk-table.html',
  styleUrl: './employees-at-risk-table.css',
})
export class EmployeesAtRiskTableComponent {}
