import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Transaction, TransactionQuery, TransactionRequest } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class TransactionApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/transactions';

  get(query: TransactionQuery): Observable<Transaction[]> {
    let params = new HttpParams().set('year', query.year).set('month', query.month);
    if (query.type) {
      params = params.set('type', query.type);
    }
    if (query.categoryId) {
      params = params.set('categoryId', query.categoryId);
    }

    return this.http.get<Transaction[]>(this.baseUrl, { params });
  }

  create(request: TransactionRequest): Observable<Transaction> {
    return this.http.post<Transaction>(this.baseUrl, request);
  }

  update(id: string, request: TransactionRequest): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
