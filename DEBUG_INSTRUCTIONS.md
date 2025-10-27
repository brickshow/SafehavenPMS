# Debug Instructions for Medication Reset and Status Changes

## Issues Found:
1. The SQL query is returning NULL - no medication orders matching the criteria
2. Possible reasons:
   - No medication orders exist in database
   - StartDate is in the future (past tomorrow)
   - DiscontinueDate has already passed
   - Status is not "Active", "InProgress", or "NotStarted"

## How to Debug:

### 1. Use the "Test Reset" Button
- Navigate to Medication > Administration Log tab
- Click the "Test Reset" button (🧪 icon)
- This will show you:
  - Total medication orders in database
  - Status breakdown (Active: X, InProgress: Y, etc.)
  - How many orders match the reset criteria
  - How many logs exist for tomorrow

### 2. Check Console/Log Output
Look for log messages like:
```
MedicationResetService: Querying for medication orders. Today: 2024-01-15, Tomorrow: 2024-01-16
Total medication orders in database: X
Found Y active medication orders after filtering
```

### 3. Verify Medication Orders Exist
Run this SQL query directly on your database:
```sql
SELECT 
    MedicationOrderId,
    Status,
    StartDate,
    DiscontinueDate,
    NoDiscontinueDate,
    Breakfast,
    Lunch,
    Dinner,
    Bedtime
FROM MedicationOrders
ORDER BY StartDate DESC
```

### 4. Common Issues and Solutions:

**Issue: "Result null"**
- **Cause**: No orders match all the criteria
- **Solution**: Create medication orders with:
  - StartDate <= tomorrow
  - Status = "Active", "InProgress", or "NotStarted"
  - NoDiscontinueDate = true OR DiscontinueDate >= tomorrow
  - At least one scheduled dose (Breakfast, Lunch, Dinner, or Bedtime)

**Issue: Status not changing**
- **Cause**: Checkboxes not being submitted correctly
- **Solution**: 
  - Check browser console for JavaScript errors
  - Verify all checkboxes have proper `name` attributes
  - Check that form submission includes anti-forgery token

**Issue: No administration logs created**
- **Cause**: Orders don't meet criteria
- **Solution**:
  - Ensure medication orders have StartDate in the past or today
  - Ensure orders are not discontinued yet
  - Click "Reset Logs" button manually to test

### 5. Manual Testing Steps:

1. **Create a Test Medication Order**:
   - StartDate = Today or earlier
   - Status = "Active"
   - At least Breakfast = true
   - NoDiscontinueDate = true

2. **Test Status Change**:
   - Go to Administration Log
   - Check a medication dose (e.g., Breakfast)
   - Save
   - Status should change to "InProgress"
   - Check console for debug messages

3. **Test Reset Service**:
   - Click "Test Reset" button
   - Note the output
   - Click "Reset Logs" button
   - Check if new logs are created

### 6. Check Application Logs:

Look for these log entries:
```
MedicationResetService: Starting medication administration log reset at midnight
Created X new administration logs for tomorrow
Medication administration log reset completed successfully
```

## Next Steps:

1. Run the application
2. Click "Test Reset" button on the Administration Log page
3. Share the output message with me
4. This will help identify what's wrong

