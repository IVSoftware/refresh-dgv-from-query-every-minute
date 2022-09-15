My suggestion is that it would make more sense to:

1. Make a background thread.
2. Perform a synchronous query on the background thread. 
3. Marshal the query result onto the UI thread to perform the DataGridView update. 
4. Synchronously wait a Task.Delay of on the background thread before performing the next query.

It is not necessary to click [Run] or to have a run button at all. Just start the polling.

***
**Initialize**
Start the polling in the `MainForm` override of the `Load` event.
```
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    initDataGridView();
    _ = startPolling(_cts.Token);
}
```

***
**Polling method**
Example shows a minimal mock query.
```
private async Task startPolling(CancellationToken token)
{
    while(!token.IsCancellationRequested)
    {
        // This is a synchronous task in the background that can take as long as it needs to.
        List<Record> mockRecordset = mockSomeDatabaseQuery();
        // Marshal onto the UI thread for the update
        BeginInvoke((MethodInvoker)delegate 
        {
            DataSource.Clear();
            foreach (var record in mockRecordset)
            {
                DataSource.Add(record);
            }
            datagridview.Refresh();
        });
        // Reduced the time interval to make it more testable.
        await Task.Delay(TimeSpan.FromSeconds(1), token);
    }
}
```

