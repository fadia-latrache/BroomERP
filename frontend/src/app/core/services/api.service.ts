import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5000/api';

  get<T>(path: string, params?: Record<string, any>): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      for (const [key, val] of Object.entries(params)) {
        if (val !== undefined && val !== null && val !== '') {
          httpParams = httpParams.set(key, String(val));
        }
      }
    }
    return this.http.get<T>(`${this.baseUrl}/${path}`, { params: httpParams });
  }

  post<T>(path: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${path}`, body);
  }

  patch<T>(path: string, body: any): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}/${path}`, body);
  }

  put<T>(path: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${path}`, body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}/${path}`);
  }

  downloadBlob(path: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${path}`, { responseType: 'blob' });
  }
}
