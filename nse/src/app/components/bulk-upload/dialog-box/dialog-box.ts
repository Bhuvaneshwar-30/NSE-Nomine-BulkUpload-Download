import { Component, HostListener, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';


@Component({
  selector: 'app-dialog-box',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  templateUrl: './dialog-box.html',
  styleUrls: ['./dialog-box.css']
})
export class DialogBox {

  displayedColumns: string[] = ['fieldSeq', 'dataItem', 'type', 'canBeBlank', 'length', 'remarks'];
  dataSource: any[] = [];
  constructor(public dialogRef: MatDialogRef<DialogBox>,
    @Inject(MAT_DIALOG_DATA) public data: any) {
    this.dataSource = data;


  }
  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: any) {
    this.dialogRef.close();
  }

  close(): void {
    this.dialogRef.close();
  }
}
