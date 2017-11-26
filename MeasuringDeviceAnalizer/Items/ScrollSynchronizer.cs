using System;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;

namespace MeasuringDeviceAnalizer
{
	/// <summary>
	/// Description of ScrollSynchronizer .
	/// </summary>
	public class ScrollSynchronizer : DependencyObject
	{		
	    public static readonly DependencyProperty ScrollGroupProperty =
		    DependencyProperty.RegisterAttached(
		    "ScrollGroup", 
		    typeof(string), 
		    typeof(ScrollSynchronizer), 
		    new PropertyMetadata(new PropertyChangedCallback(
		    OnScrollGroupChanged)));
	
    	public static void SetScrollGroup(DependencyObject obj, string scrollGroup)
    	{
    	    obj.SetValue(ScrollGroupProperty, scrollGroup);
    	}
	
    	public static string GetScrollGroup(DependencyObject obj)
    	{
    	    return (string)obj.GetValue(ScrollGroupProperty);
    	}	

		private static Dictionary<ScrollViewer, string> scrollViewers = 
		    new Dictionary<ScrollViewer, string>();

		private static Dictionary<string, double> verticalScrollOffsets = 
	    	new Dictionary<string, double>();    	
		
		private static void OnScrollGroupChanged(DependencyObject d, 
                    DependencyPropertyChangedEventArgs e)
		{
		    var scrollViewer = d as ScrollViewer;
		    if (scrollViewer != null)
    		{		
        		if (!string.IsNullOrEmpty((string)e.OldValue))
        		{
        		    // Remove scrollviewer
        		    if (scrollViewers.ContainsKey(scrollViewer))
        		    {
        		        scrollViewer.ScrollChanged -= 
        			          new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
		                scrollViewers.Remove(scrollViewer);
            		}
        		}

        		if (!string.IsNullOrEmpty((string)e.NewValue))
        		{		
            		// If group already exists, set scrollposition of 
            		// new scrollviewer to the scrollposition of the group		
        		    if (verticalScrollOffsets.ContainsKey((string)e.NewValue))
        		    {
        		        scrollViewer.ScrollToVerticalOffset(verticalScrollOffsets[(string)e.NewValue]);
        		    }
        		    else
        		    {
        		        verticalScrollOffsets.Add((string)e.NewValue, scrollViewer.VerticalOffset);
        		    }

        		    // Add scrollviewer
        		    scrollViewers.Add(scrollViewer, (string)e.NewValue);
        		    scrollViewer.ScrollChanged += 
        		        new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
        		}
		    }
		}	

		private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
    		if (e.VerticalChange != 0)
    		{
        		var changedScrollViewer = sender as ScrollViewer;
        		Scroll(changedScrollViewer);
		    }
		}

		private static void Scroll(ScrollViewer changedScrollViewer)
		{
		    var group = scrollViewers[changedScrollViewer];
		    verticalScrollOffsets[group] = changedScrollViewer.VerticalOffset;

    		foreach (var scrollViewer in scrollViewers)
    		{
    			if ((scrollViewer.Value == group) && (scrollViewer.Key != changedScrollViewer))
    			{
	        		if (scrollViewer.Key.VerticalOffset != changedScrollViewer.VerticalOffset)
    	    		{
    	    		    scrollViewer.Key.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
    	    		}			
    			}
		    }
		}		
	}
}
