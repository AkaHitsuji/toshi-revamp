//
//  ListSubViewController.m
//  Unity-iPhone
//
//  Created by Yang Ang on 11/10/18.
//

#import "ListSubViewController.h"
#import "ListTableViewCell.h"

@interface ListSubViewController ()

@end

@implementation ListSubViewController {
    NSArray *tableData;
    NSArray *consolidatedListTitle;
    NSArray *consolidatedListQty;
    NSArray *consolidatedListPrice;
    NSArray *consolidatedListStatus;
}

- (void)viewDidLoad {
    [super viewDidLoad];
    tableData = [NSArray arrayWithObjects:@"Egg Benedict", @"Mushroom Risotto", @"Full Breakfast", @"Hamburger", @"Ham and Egg Sandwich", @"Creme Brelee", @"White Chocolate Donut", @"Starbucks Coffee", @"Vegetable Curry", @"Instant Noodle with Egg", @"Noodle with BBQ Pork", @"Japanese Noodle with Pork", @"Green Tea", @"Thai Shrimp Cake", @"Angry Birds Cake", @"Ham and Cheese Panini", nil];
    
    [self getData];
    // Do any additional setup after loading the view.
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (nonnull UITableViewCell *)tableView:(nonnull UITableView *)tableView cellForRowAtIndexPath:(nonnull NSIndexPath *)indexPath {
    static NSString *simpleTableIdentifier = @"SimpleTableItem";
    
    ListTableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:simpleTableIdentifier];
    
    if (cell == nil) {
        cell = [[ListTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:simpleTableIdentifier];
    }
    
    cell.itemName.text = [consolidatedListTitle objectAtIndex:indexPath.row];
    cell.price.text = [NSString stringWithFormat:@"$%@", [consolidatedListPrice objectAtIndex:indexPath.row]];
    cell.qty.text = [NSString stringWithFormat:@"Qty: %@", [consolidatedListQty objectAtIndex:indexPath.row]];

    if ([[consolidatedListStatus objectAtIndex:indexPath.row] isEqualToString:@"in_cart"]) {
        // green color
        cell.backgroundColor = [UIColor colorWithRed:132.0/255.0 green:218.0/255.0 blue:0.0/255.0 alpha:1];
    }
    else {
        // orange color
        cell.backgroundColor = [UIColor colorWithRed:249.0/255.0 green:183.0/255.0 blue:68.0/255.0 alpha:1];
    }
    
    return cell;
}

- (NSInteger)tableView:(nonnull UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return [consolidatedListPrice count];
}

-(void)getData {
    NSError *error;
    NSString *url_string = [NSString stringWithFormat: @"http://revamp.ap-southeast-1.elasticbeanstalk.com/user/toshi/shopping"];
    NSData *data = [NSData dataWithContentsOfURL: [NSURL URLWithString:url_string]];
    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&error];
    NSDictionary *itemData = [json valueForKey:@"items"];
    
    consolidatedListTitle = [itemData valueForKey:@"name"];
    consolidatedListQty = [itemData valueForKey:@"qty"];
    consolidatedListPrice = [itemData valueForKey:@"price"];
//    NSLog(@"price: %@",consolidatedListPrice);
//    NSLog(@"titles: %@",consolidatedListTitle);
//    NSLog(@"qty: %@",consolidatedListQty);
    
    consolidatedListStatus = [itemData valueForKey:@"status"];
//    NSLog(@"status: %@",consolidatedListStatus);
}

- (IBAction)closeView:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}

- (IBAction)checkoutView:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}
@end
