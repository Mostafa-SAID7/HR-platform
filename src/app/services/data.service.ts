import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, delay, map } from 'rxjs';

/**
 * Data Service
 * 
 * Simulates API calls by fetching local JSON files from the public/api directory.
 * Includes artificial delay to verify loading states and UX.
 */

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  /**
   * Fetch Dashboard Metrics
   */
  getDashboardMetrics(): Observable<any> {
    return this.http.get(`${this.baseUrl}/dashboard-metrics.json`).pipe(
      delay(500) // Simulate network latency
    );
  }

  /**
   * Fetch Workforce Metrics
   */
  getWorkforceMetrics(): Observable<any> {
    return this.http.get(`${this.baseUrl}/workforce-metrics.json`).pipe(
      delay(600)
    );
  }

  /**
   * Fetch Performance Metrics
   */
  getPerformanceMetrics(): Observable<any> {
    return this.http.get(`${this.baseUrl}/performance-metrics.json`).pipe(
      delay(700)
    );
  }

  /**
   * Fetch Turnover Metrics
   */
  getTurnoverMetrics(): Observable<any> {
    return this.http.get(`${this.baseUrl}/turnover-metrics.json`).pipe(
      delay(550)
    );
  }

  /**
   * Fetch Hiring Forecast
   */
  getHiringForecast(): Observable<any> {
    return this.http.get(`${this.baseUrl}/hiring-forecast.json`).pipe(
      delay(800)
    );
  }

  /**
   * Fetch All Employees
   */
  getEmployees(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/employees.json`).pipe(
      delay(400)
    );
  }

  /**
   * Fetch Report Templates
   */
  getReportTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/report-templates.json`).pipe(
      delay(500)
    );
  }

  /**
   * Fetch Recruitment Metrics
   * (Alias for Hiring Forecast in this simulation)
   */
  getRecruitmentMetrics(): Observable<any> {
    return this.getHiringForecast();
  }
}
