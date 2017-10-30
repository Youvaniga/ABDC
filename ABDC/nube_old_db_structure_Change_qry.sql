ALTER TABLE AccountGroups ALTER COLUMN AccountGroupId numeric(18,0) not null
ALTER TABLE AccountGroups ALTER COLUMN Under numeric(18,0) 
ALTER TABLE Ledger alter column LedgerID numeric(18,0) not null
ALTER TABLE LedgerOP alter column LedgerOPID numeric(18,0) not null
ALTER TABLE LedgerOP alter column LedgerID numeric(18,0) not null
ALTER TABLE JournalMaster alter column JournalId numeric(18,0) not null
ALTER TABLE JournalDetails alter column JDId numeric(18,0) not null
ALTER TABLE PaymentMaster alter column PaymentId numeric(18,0) not null
ALTER TABLE PaymentDetails alter column PDID numeric(18,0) not null
ALTER TABLE ReceiptMaster alter column ReceiptId numeric(18,0) not null
ALTER TABLE ReceiptDetails alter column RDID numeric(18,0) not null

ALTER TABLE Ledger ADD primary key(LedgerId)
ALTER TABLE LedgerOP ADD primary key(LedgerOPID)
ALTER TABLE JournalMaster ADD primary key(JournalId)
alter table JournalDetails add primary key(JDId)
ALTER TABLE PaymentMaster ADD primary key(PaymentId)
alter table PaymentDetails add primary key(PDID)
ALTER TABLE ReceiptMaster ADD primary key(ReceiptId)
alter table ReceiptDetails add primary key(RDID)

delete from LedgerOP where not LedgerID in (select LedgerID from Ledger)
delete from PaymentDetails where PaymentId in (select PaymentId from PaymentMaster where not LedgerId in (select LedgerId from Ledger))
delete from PaymentMaster where not LedgerId in (select LedgerId from Ledger)

ALTER TABLE AccountGroups ADD FOREIGN KEY(Under) references AccountGroups(AccountGroupId)                               
ALTER TABLE Ledger ADD FOREIGN KEY (AccountGroupId) REFERENCES AccountGroups(AccountGroupId)
alter table LedgerOP add foreign key(LedgerID) references Ledger(LedgerId)
alter table journalDetails add foreign key(JournalID) references JournalMaster(JournalID)
alter table journalDetails add foreign key(LedgerID) references Ledger(LedgerId)
alter table PaymentMaster add foreign key(LedgerId) references Ledger(LedgerId)
alter table PaymentDetails add foreign key(PaymentId) references PaymentMaster(PaymentId)
alter table PaymentDetails add foreign key(LedgerID) references Ledger(LedgerId)
alter table ReceiptMaster add foreign key(LedgerId) references Ledger(LedgerId)
alter table ReceiptDetails add foreign key(ReceiptId) references ReceiptMaster(ReceiptId)
alter table ReceiptDetails add foreign key(LedgerID) references Ledger(LedgerId)