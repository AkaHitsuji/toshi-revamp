//
//  CheckoutViewController.h
//  Unity-iPhone
//
//  Created by Yang Ang on 11/10/18.
//

#import <UIKit/UIKit.h>

@interface CheckoutViewController : UIViewController
- (IBAction)closeCheckout:(id)sender;
@property (weak, nonatomic) IBOutlet UILabel *s_price;
@property (weak, nonatomic) IBOutlet UILabel *d_price;
@property (weak, nonatomic) IBOutlet UILabel *t_price;


@end
