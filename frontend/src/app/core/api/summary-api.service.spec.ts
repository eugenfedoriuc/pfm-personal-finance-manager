import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SummaryApiService } from './summary-api.service';

describe('SummaryApiService', () => {
  let service: SummaryApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(SummaryApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requests the summary for the selected year and month', () => {
    const response = {
      year: 2026,
      month: 9,
      totalIncome: 0,
      totalExpenses: 0,
      balance: 0,
      incomeBreakdown: [],
      expenseBreakdown: [],
      budgetComparison: [],
      topExpenseCategory: null,
      dailyExpenses: [],
    };

    service.get(2026, 9).subscribe((summary) => expect(summary).toEqual(response));

    const request = http.expectOne('/api/summaries/2026/9');
    expect(request.request.method).toBe('GET');
    request.flush(response);
  });
});
