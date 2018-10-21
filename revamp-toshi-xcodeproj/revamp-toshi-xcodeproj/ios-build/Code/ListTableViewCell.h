//
//  ListTableViewCell.h
//  Unity-iPhone
//
//  Created by Yang Ang on 11/10/18.
//

#import <UIKit/UIKit.h>

@interface ListTableViewCell : UITableViewCell

@property (weak, nonatomic) IBOutlet UILabel *price;
@property (weak, nonatomic) IBOutlet UILabel *itemName;
@property (weak, nonatomic) IBOutlet UILabel *qty;

@end
