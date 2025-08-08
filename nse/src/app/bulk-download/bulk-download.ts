import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { NomineeData } from '../models/nominee-data.model';// Adjust the import path as necessary
import * as XLSX from 'xlsx';
import * as FileSaver from 'file-saver';
import { NomineServices } from '../sharedservice'


@Component({
  selector: 'app-bulk-download',
  imports: [FormsModule, CommonModule],
  standalone: true,
  templateUrl: './bulk-download.html',
  styleUrls: ['./bulk-download.css']
})


export class BulkDownloadComponent {

  fromDate: string = '';
  toDate: string = '';
  customerId: string = '';
  nomineeData: NomineeData[] | null = null;
  loading = false;
  searchAttempted: boolean = false;


  constructor(private http: HttpClient , private nomineservices: NomineServices) { }

  reset(from: any) {
    from.reset();
    console.log('Form reset');
    this.nomineeData = null;
    this.searchAttempted = false;
  }

  formatToIsoDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  locate() {
   
    if (!this.validation()) return;
     this.searchAttempted = true;
    this.loading = true;
    this.nomineeData = null;
    const params = new HttpParams()
      .set('customerId', this.customerId)
      .set('searchStart', this.fromDate)
      .set('searchEnd', this.toDate);
    console.log('API Params:', params);

    this.nomineservices.getnominee(this.customerId,this.fromDate,this.toDate).subscribe({
      next: (data) => {
        if (Array.isArray(data) && data.length) {
          this.nomineeData = data;
        } else {
          this.nomineeData = [];
        }
      },
      error: (err) => {
        console.error('API Error:', err);
        this.nomineeData = [];
        alert('Error fetching data: ' + err.message);
      }
    });
  }


  download() {
    if (!this.validation()){
       return;
    }

    if (!this.nomineeData || this.nomineeData.length === 0) {
      this.searchAttempted = true;
      return;
    }
    this.searchAttempted = false;
    const exportData = this.nomineeData.map((item: any) => ({
      'Customer ID': item.customerID,
      'Investor Name': item.investorName,
      'POA Execution': item.executeThroughPoa,
      'Bank Name': item.bankName,
      'Account Number': item.accountNumber,
      'Branch Name': item.branchName,
      'Account Type': item.accountType,
      'MICR Number': item.micrNumber,
      'IFSC Code': item.ifscCode,
      'Bank Holder Name': item.bankHolderName,
      'Bank Holder Name 1': item.bankHolderName1,
      'Bank Holder Name 2': item.bankHolderName2,
      'ACH Amount': item.achAmount,
      'ACH From Date': this.formatToIsoDate(item.achFromDate),
      'ACH To Date': this.formatToIsoDate(item.achToDate),
      'mode_of_holder': item.modeOfHolder
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(exportData);
    const workbook: XLSX.WorkBook = { Sheets: { 'NomineeData': worksheet }, SheetNames: ['NomineeData'] };
    const excelBuffer: any = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });

    const blobData: Blob = new Blob([excelBuffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8'
    });

    const fileName = `NomineeData_${this.customerId}_${this.fromDate}_to_${this.toDate}.xlsx`;
    FileSaver.saveAs(blobData, fileName);
  }

 validation(): boolean {
  const missingFields: string[] = [];

  if (
    this.customerId === null ||
    this.customerId === undefined ||
    this.customerId.toString().trim() === '' ||
    isNaN(Number(this.customerId))
  ) {
    missingFields.push('Customer ID');
  }

  if (!this.fromDate || isNaN(Date.parse(this.fromDate))) {
    missingFields.push('From Date');
  }

  if (!this.toDate || isNaN(Date.parse(this.toDate))) {
    missingFields.push('To Date');
  }

  if (missingFields.length > 0) {
    alert('Missing or Invalid: ' + missingFields.join(', '));
    return false;
  }

  const from = new Date(this.fromDate);
  const to = new Date(this.toDate);
  if (from > to) {
    alert('From Date cannot be later than To Date.');
    return false;
  }

  return true;
}



  allowOnlyNumbers(event: KeyboardEvent): void {
  const allowedKeys = ['Backspace', 'Tab', 'ArrowLeft', 'ArrowRight', 'Delete'];

  if (allowedKeys.includes(event.key)) {
    return; 
  }

  if (!/^\d$/.test(event.key)) {
    event.preventDefault();
  }
}


  sanitizePaste(event: ClipboardEvent): void {
    const pastedInput: string = event.clipboardData?.getData('text') ?? '';
    if (!/^\d+$/.test(pastedInput)) {
      event.preventDefault();
    }
  }
  get currentNominee(): NomineeData | null {
    return this.nomineeData?.[0] || null;
  }


}
