  import { HttpClient , HttpParams} from '@angular/common/http';
  import { Injectable } from '@angular/core';
  import { Observable } from 'rxjs';
    import { NomineeData } from './models/nominee-data.model';

@Injectable({
    providedIn: 'root'
})

export class NomineServices{
    private baseurl = 'https://localhost:7153/api/Nomine/GetNomineeData';

    constructor(private http: HttpClient) {}

    getnominee( customerID: string, fromDate: string, toDate: string): Observable<NomineeData[]>{
    const params = new HttpParams()
      .set('customerId', customerID)
      .set('searchStart',fromDate)
      .set('searchEnd',toDate);

      return this.http.get<NomineeData[]>(this.baseurl, { params });
    }
}
