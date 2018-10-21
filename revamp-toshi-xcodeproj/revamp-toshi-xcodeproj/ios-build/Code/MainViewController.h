//
//  MainViewController.h
//
//  Created by Yang Ang on 7/10/18.

#import <UIKit/UIKit.h>
#import "UnityAppController.h"
#import "UI/UnityView.h"
#import "UI/UnityViewControllerBase.h"

@interface MainViewController : UIViewController <UICollectionViewDataSource, UICollectionViewDelegate, UITableViewDataSource, UITableViewDelegate>
{
    UnityDefaultViewController *unityViewController;
    UnityAppController *unityController;
}

-(IBAction) touchToLoad:(id)sender;

@end
