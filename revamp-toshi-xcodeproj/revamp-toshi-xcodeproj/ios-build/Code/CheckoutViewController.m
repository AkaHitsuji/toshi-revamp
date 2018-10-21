//
//  CheckoutViewController.m
//  Unity-iPhone
//
//  Created by Yang Ang on 11/10/18.
//

#import "CheckoutViewController.h"
#import "ListTableViewCell.h"

@interface CheckoutViewController ()

@end

@implementation CheckoutViewController {
    NSArray *tableData;
    NSArray *consolidatedListTitle;
    NSArray *consolidatedListQty;
    NSArray *consolidatedListPrice;
}

- (void)viewDidLoad {
    [super viewDidLoad];
    [self getData];
    tableData = [NSArray arrayWithObjects:@"Egg Benedict", @"Mushroom Risotto", @"Full Breakfast", @"Hamburger", @"Ham and Egg Sandwich", @"Creme Brelee", @"White Chocolate Donut", @"Starbucks Coffee", @"Vegetable Curry", @"Instant Noodle with Egg", @"Noodle with BBQ Pork", @"Japanese Noodle with Pork", @"Green Tea", @"Thai Shrimp Cake", @"Angry Birds Cake", @"Ham and Cheese Panini", nil];
    // Do any additional setup after loading the view.
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (nonnull UITableViewCell *)tableView:(nonnull UITableView *)tableView cellForRowAtIndexPath:(nonnull NSIndexPath *)indexPath {
    static NSString *simpleTableIdentifier = @"CheckoutTableItem";
    
    ListTableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:simpleTableIdentifier];
    
    if (cell == nil) {
        cell = [[ListTableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:simpleTableIdentifier];
    }
    
    cell.itemName.text = [consolidatedListTitle objectAtIndex:indexPath.row];
    cell.price.text = [NSString stringWithFormat:@"$%@", [consolidatedListPrice objectAtIndex:indexPath.row]];
    cell.qty.text = [NSString stringWithFormat:@"Qty: %@", [consolidatedListQty objectAtIndex:indexPath.row]];
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
    
    _d_price.text = [NSString stringWithFormat:@"%.02f", [[json valueForKey:@"discount"]floatValue]];
    _s_price.text = [NSString stringWithFormat:@"%.02f", [[json valueForKey:@"subtotal"]floatValue]];
    _t_price.text = [NSString stringWithFormat:@"%.02f", [[json valueForKey:@"total"]floatValue]];
}

- (IBAction)closeCheckout:(id)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
}
@end
