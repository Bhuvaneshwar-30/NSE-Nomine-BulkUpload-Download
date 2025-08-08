import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, inject, viewChild } from '@angular/core';
import * as XLSX from 'xlsx';
import { NomineeData } from '../../models/nominee-data.model';
import { MatDialog } from '@angular/material/dialog';
import { DialogBox } from './dialog-box/dialog-box';


@Component({
  selector: 'app-bulk-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bulk-upload.html',
  styleUrls: ['./bulk-upload.css']
})

export class BulkUploadComponent {
  readonly fileInputRef = viewChild.required<ElementRef>('fileInput');

  selectedFile: File | null = null;
  excelHeaders: string[] = [];
  excelData: any[] = [];
  CustomerID: number = 0;
  Preview: boolean = false;



  constructor(private http: HttpClient,private dialog: MatDialog) { }

  ngOnInit() {
  }


  ngAfterViewInit() {
    console.log('fileinputRef initialized:', this.fileInputRef());
  }


  triggerFileInput() {
    const fileInputRef = this.fileInputRef();
    if (fileInputRef) {
      fileInputRef.nativeElement.click();
    } else {
      console.error('fileInputRef is not initialized.');
    }
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      const allowedFiles = ['.txt', '.xlsx', '.xls'];
      const extension = file.name.split('.').pop()?.toLowerCase();

      if (!extension || !allowedFiles.includes(`.${extension}`)) {
        alert('Please upload a valid file with .txt, .xlsx, or .xls extension.');
        return;
      }
      this.selectedFile = file;
      this.excelHeaders = [];
      this.excelData = [];
    } else {
      this.selectedFile = null;
      console.log('No file selected');
    }
  }

  importbtn() {
    if (!this.selectedFile) {
      alert('Please select a file first.');
      return;
    }
    console.log('Sending to backend:');

    console.log('File:', this.selectedFile);
    const formData = new FormData();

    formData.append('File', this.selectedFile);


    console.log('Sending File:', this.selectedFile.name);

    this.http.post('https://localhost:7153/api/Nomine', formData, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          console.log('Server response:', response);
          alert(`Upload and DB insert successful.: ${response}`);
        },
        error: (error) => {
          console.error('Upload failed', error);
          alert(`Upload failed: ${error.error}`);
        }
      });

  }
  
 public structure():void {
   
  const demo = [
      { fieldSeq: 1, dataItem: 'Customer ID', type: 'Number', canBeBlank: 'No', length: 10, remarks: 'Unique identifier for the investor' },
      { fieldSeq: 2, dataItem: 'Investor Name', type: 'character', canBeBlank: 'No', length: 50, remarks: 'Enter full legal name as per PAN' },
      { fieldSeq: 3, dataItem: 'POA Execution', type: 'character', canBeBlank: 'No', length: 10, remarks: 'Power of Attorney status (Yes/No)' },
      { fieldSeq: 4, dataItem: 'Bank Name', type: 'character', canBeBlank: 'No', length: 100, remarks: 'Enter registered bank name' },
      { fieldSeq: 5, dataItem: 'Account Number', type: 'Number', canBeBlank: 'No', length: 10, remarks: 'Bank account number for ACH debit' },
      { fieldSeq: 6,dataItem:'Branch Name', type: 'character',canBeBlank:'No',length: 100,remarks:'Bank branch where account is held'},
      { fieldSeq: 7,dataItem:'Account Type',type: 'character',canBeBlank:'No',length: 10,remarks:'Savings/Current/Other'},
      { fieldSeq: 8,dataItem:'MICR Number',type: 'Number',canBeBlank:'No',length: 10,remarks:'9-digit MICR for bank branch'},
      { fieldSeq: 9,dataItem:'IFSC Code',type: 'char and num',canBeBlank:'No',length: 10,remarks:'11-character IFSC code'},
      { fieldSeq: 10,dataItem:'Bank Holder Name',type: 'character',canBeBlank:'No',length: 100,remarks:'Primary account holder name'},
      { fieldSeq: 11,dataItem:'Bank Holder Name 1',type: 'character',canBeBlank:'Yes',length: 100,remarks:'Joint holder (if any)'},
      { fieldSeq: 12,dataItem:'Bank Holder Name 2',type: 'character',canBeBlank:'Yes',length: 100,remarks:'Second joint holder (if any)'},
      { fieldSeq: 13,dataItem:'ACH Amount',type: 'Number',canBeBlank:'No',length: 10,remarks:'Max amount to debit via ACH'},
      { fieldSeq: 14,dataItem:'ACH From Date',type: 'Number',canBeBlank:'No',length: 10,remarks:'Start date of ACH mandate (YYYYMMDD)'},
      { fieldSeq: 15,dataItem:'ACH To Date',type: 'Number',canBeBlank:'No',length: 10,remarks:'End date of ACH mandate (YYYYMMDD)'},
      { fieldSeq: 16,dataItem:'mode_of_holde',type: 'character',canBeBlank:'No',length: 10,remarks:'Holding mode: Single/Joint/Anyone'}
  ];

  this.dialog.open(DialogBox,{
    data: demo,
    panelClass:'custom-dialog-cont',
    disableClose: true 
  });


  console.log("Structure button clicked");
    
  }
  preview(): void {
    this.excelData = [];
    this.excelHeaders = [];
    this.Preview = false;
  }

}


