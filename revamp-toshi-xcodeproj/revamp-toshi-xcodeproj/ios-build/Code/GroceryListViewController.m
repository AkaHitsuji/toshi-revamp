//
//  GroceryListViewController.m
//  Unity-iPhone
//
//  Created by Yang Ang on 11/10/18.
//

#import "GroceryListViewController.h"
#import "ListTableViewCell.h"

@interface GroceryListViewController ()

@end

@implementation GroceryListViewController {
    NSArray *tableData;
}

- (void)viewDidLoad {
    [super viewDidLoad];
    tableData = [NSArray arrayWithObjects:@"Egg Benedict", @"Mushroom Risotto", @"Full Breakfast", @"Hamburger", @"Ham and Egg Sandwich", @"Creme Brelee", @"White Chocolate Donut", @"Starbucks Coffee", @"Vegetable Curry", @"Instant Noodle with Egg", @"Noodle with BBQ Pork", @"Japanese Noodle with Pork", @"Green Tea", @"Thai Shrimp Cake", @"Angry Birds Cake", @"Ham and Cheese Panini", nil];
    // Do any additional setup after loading the view.
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (nonnull UITableViewCell *)tableView:(nonnull UITableView *)tableView cellForRowAtIndexPath:(nonnull NSIndexPath *)indexPath {
    static NSString *simpleTableIdentifier = @"GroceryTableItem";
    
    ListTableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:simpleTableIdentifier];
    
    if (cell == nil) {
        cell = [[ListTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:simpleTableIdentifier];
    }
    
    NSLog(@"Grocery data is:%@",[tableData objectAtIndex:indexPath.row]);
    cell.itemName.text = [tableData objectAtIndex:indexPath.row];
    cell.price.text = @"$3.00";
    cell.qty.text = @"Qty: 3";
    cell.backgroundColor = [UIColor whiteColor];
    return cell;
}

- (NSInteger)tableView:(nonnull UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return [tableData count];
}

- (IBAction)addList:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}

- (IBAction)closeView:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}
@end
