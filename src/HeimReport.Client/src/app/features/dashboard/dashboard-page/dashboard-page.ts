import { Component } from "@angular/core";
import { SidebarComponent } from './components/sidebar/sidebar';
import { TopbarComponent } from './components/topbar/topbar';
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
    JourneyFlowComponent, RiskDistributionComponent, TrendChartComponent, EmployeesAtRiskTableComponent     ],
    templateUrl: "./dashboard-page.html",
    styleUrls: ["./dashboard-page.css"]
})
export class DashboardPageComponent {}