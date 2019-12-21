class CheckingAccount
{
  // data-declarations
  private String accountNumber;
  private String accountHolder;
  private int    balance;
  private int    useCount = 0;

  //constructors
  CheckingAccount( String accNumber, String holder, int start )
  {
    accountNumber = accNumber ;
    accountHolder = holder ;
    balance       = start ;
  }

  // methods
  private void incrementUse()
  {
    useCount = useCount + 1;
  }

  int getBalance()
  {
    return balance ;
  }

  void  processDeposit( int amount )
  {
    incrementUse();
    balance = balance + amount ; 
  }

  void processCheck( int amount )
  {
    int charge;

    incrementUse();
    if ( balance < 100000 )
      charge = 15; 
    else
      charge = 0;

    balance =  balance - amount - charge  ;
  }

  public String toString()
  {
    return  "Account: " + accountNumber + "\tName: " + accountHolder + 
            "\tBalance: " +  balance + "\tUse Count: " + useCount;
  }

}