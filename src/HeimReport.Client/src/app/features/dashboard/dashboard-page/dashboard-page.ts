import { Component } from "@angular/core";
import { ButtonComponent } from '../../../shared/components/button/button';
import { SidebarComponent } from '../../../shared/components/sidebar/sidebar';
import { TopbarComponent } from '../../../shared/components/topbar/topbar';
import { MetricCardComponent } from './components/metric-card/metric-card';
import { ChurnDriversComponent } from './components/churn-drivers/churn-drivers';
import { JourneyFlowComponent } from './components/journey-flow/journey-flow';
import { RiskDistributionComponent } from './components/risk-distribution/risk-distribution';
import { TrendChartComponent } from './components/trend-chart/trend-chart';
import { EmployeesAtRiskTableComponent } from './components/employees-at-risk-table/employees-at-risk-table';
@Component({
    selector: "app-dashboard-page",
    standalone: true,
    imports: [
    SidebarComponent, TopbarComponent, MetricCardComponent, ChurnDriversComponent,
    JourneyFlowComponent, RiskDistributionComponent, TrendChartComponent, EmployeesAtRiskTableComponent, ButtonComponent ],
    templateUrl: "./dashboard-page.html",
    styleUrls: ["./dashboard-page.css"]
})
export class DashboardPageComponent {}