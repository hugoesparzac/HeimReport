import { Component } from '@angular/core';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TopbarComponent } from '../../../shared/components/topbar/topbar';
import { PageBackgroundComponent } from '../../../shared/components/page-background/page-background';
import { ProfileSummaryCardComponent } from './components/profile-summary-card/profile-summary-card';
import { AttritionRiskGaugeComponent } from './components/attrition-risk-gauge/attrition-risk-gauge';
import { LifecycleMetricsCardComponent } from './components/lifecycle-metrics-card/lifecycle-metrics-card';
import { ComprehensiveDataCardComponent } from './components/comprehensive-data-card/comprehensive-data-card';
import { SentimentTrendCardComponent } from './components/sentiment-trend-card/sentiment-trend-card';
import { TerminationDangerZoneComponent } from './components/termination-danger-zone/termination-danger-zone';

@Component({
  selector: 'app-employee-detail-page',
  standalone: true,
  imports: [
    SidebarComponent,
    TopbarComponent,
    PageBackgroundComponent,
    ProfileSummaryCardComponent,
    AttritionRiskGaugeComponent,
    LifecycleMetricsCardComponent,
    ComprehensiveDataCardComponent,
    SentimentTrendCardComponent,
    TerminationDangerZoneComponent,
  ],
  templateUrl: './employee-detail-page.html',
})
export class EmployeeDetailPageComponent {}