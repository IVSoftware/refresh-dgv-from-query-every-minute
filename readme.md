Instead of a while loop, it might make more sense to:

1. Make an async `Task`.
2. Perform a synchronous query inside the `Task`. 
3. Marshal the query result onto the UI thread to perform the DataGridView update. 
4. Await a Task.Delay inside the `Task` before performing the next query.

It is not necessary to click [Run] or to have a run button at all, just start the polling. Unlike a timer loop, it doesn't matter how long a query takes, it just starts a new interval delay whenever the query gets finished executing.

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
        // Example uses a reduced time interval to make it more testable.
        await Task.Delay(TimeSpan.FromSeconds(1), token);
    }
}
```

![Screenshot](https://github.com/IVSoftware/refresh-dgv-from-query-every-minute/blob/master/refresh_dgv_from_query_every_minute/Screenshots/screenshot.png)

