import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkDownloadComponent } from './bulk-download';

describe('BulkDownload', () => {
  let component: BulkDownloadComponent;
  let fixture: ComponentFixture<BulkDownloadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BulkDownloadComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BulkDownloadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
